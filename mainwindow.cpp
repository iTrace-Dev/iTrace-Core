#include "mainwindow.h"
#include <QFileDialog>
#include <QtNetwork>
#include <QApplication>
#include <QMessageBox>
#include <QDebug>
#include <QString>
#include "ui_mainwindow.h"
#include "mousetracker.h"
#include "tobiiEyetracker.h"
#include "reticle.h"
#include "calibrationscreen.h"
#include "tracker.h"
#include "xmlwriter.h"
#include "sessionsetup.h"
#include <sstream>


using namespace std;

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    mouseTracker = new MouseTracker();
    ui->setupUi(this);
    connect(ui->startServerButton, SIGNAL(released()), this, SLOT(startTracker()));
    connect(ui->reticleBox, SIGNAL(stateChanged(int)), this, SLOT(toggleReticle()));
    connect(ui->calibrateButton, SIGNAL(released()), this, SLOT(startCalibration()));
    connect(ui->sessionButton, SIGNAL(released()), this, SLOT(showSessionSetup()));
    socket = new QTcpSocket(this);
    connect(socket, SIGNAL(readyRead()), this, SLOT(displayData()));
    this->setFixedSize(this->geometry().width(),this->geometry().height());
    ui->trackerBox->addItem("Mouse Tracker");
    for(int i=0; i<TobiiEyeTracker::tobiiEyeTrackers.size(); i++){
        ui->trackerBox->addItem(TobiiEyeTracker::tobiiEyeTrackers[i]->getName());
    }
    timer = 0;
}

MainWindow::~MainWindow()
{
    delete mouseTracker;
    delete socket;
    delete ui;
}

void MainWindow::startTracker(){
    if(ui->xmlCheck->isChecked() || ui->jsonCheck->isChecked()){
        if(ui->trackerBox->currentIndex() == 0){
            if(ui->startServerButton->text() == "Start Server"){
                mouseTracker->start();
                xmlwrite.setFile();
                xmlwrite.writeEnvironment();
            }
         }
        else{
            qDebug() << ui->trackerBox->currentIndex() << " " << TobiiEyeTracker::tobiiEyeTrackers.size();
             TobiiEyeTracker::tobiiEyeTrackers[ui->trackerBox->currentIndex()-1]->startTracker();
             xmlwrite.setFile();
             xmlwrite.writeEnvironment();
         }


    if(ui->startServerButton->text() == "Start Server"){
       ui->startServerButton->setText("Stop Server");
       size = 0; // has no use?
       socket->connectToHost("localhost",8080,QIODevice::ReadOnly);
    }
    else if(ui->startServerButton->text() == "Stop Server"){
        QMessageBox::StandardButton reply;
            reply = QMessageBox::question(this, "Alert", "Do you want to stop the server?");
            if (reply == QMessageBox::Yes){
             ui->startServerButton->setText("Start Server");
             /*need to stop tracker here */
             //TobiiEyeTracker::tobiiEyeTrackers[ui->trackerBox->currentIndex()-1]->disconnect());
             xmlwrite.closeFile();
             mouseTracker->stop();
             socket->disconnectFromHost();
             ui->textBrowser->clear();


            }
    }


}
else{
        QMessageBox::StandardButton r;
        r = QMessageBox::warning(this, "Alert", "Please select JSON or XML or both to start tracking!");
  }

}

void MainWindow::toggleReticle(){
    Reticle* r = Reticle::getReticle();
    if(ui->reticleBox->isChecked()){
        r->show();
    }else{
        r->hide();
    }
}

void MainWindow::displayData(){
    timer = (timer+1)%10;
    QDataStream in(socket);
    in.setVersion(QDataStream::Qt_5_8);
    char * data = new char[100];
    in.readRawData(data,100);
    if(timer == 0)ui->textBrowser->setText(data);
   // gaze.setGaze(data)
    xmlwrite.writeResponse(data);
    //xmlwrite.writeGaze(data);
    delete data;
}
void MainWindow::showSessionSetup(){

}


void MainWindow::startCalibration(){
    CalibrationScreen* calibrationScreen = CalibrationScreen::getCalibrationScreen();
    Tracker* tracker;
    if(ui->trackerBox->currentIndex() == 0) tracker = (Tracker*)mouseTracker;
    else tracker = (Tracker*)TobiiEyeTracker::tobiiEyeTrackers[ui->trackerBox->currentIndex()-1];
    calibrationScreen->startCalibration(tracker);
}
