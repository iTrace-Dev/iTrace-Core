#include "mainwindow.h"
#include <QFileDialog>
#include <QtNetwork>
#include <QApplication>
#include <QMessageBox>
#include <QDebug>
#include <QDialog>
#include <QString>
#include "ui_mainwindow.h"
#include "mousetracker.h"
#include "tobiiEyetracker.h"
#include "reticle.h"
#include "calibrationscreen.h"
#include "tracker.h"
#include "xmlwriter.h"
#include "ui_sessionsetup.h"
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
                mouseTracker->startTracker();
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
             if(ui->trackerBox->currentIndex() == 0) mouseTracker->stopTracker();
             else TobiiEyeTracker::tobiiEyeTrackers[ui->trackerBox->currentIndex()-1]->stopTracker();
             xmlwrite.closeFile();
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
    int counter = 0, i=0;
    stringstream ss;
    QDataStream in(socket);
    in.setVersion(QDataStream::Qt_5_8);
    char * data = new char[100];
    string tmp;
    in.readRawData(data,100);
    ss << data;
    if(timer == 0)
    {
        /*while(counter!=2){
            if(data[i] = ',')counter++;
            i++;
        }
        tmp = ss.str().substr(0,i-1);*/
        ui->textBrowser->setText(data);
        xmlwrite.writeResponse(data);
    }
    //xmlwrite.writeGaze(data);
    delete data;
}
void MainWindow::showSessionSetup(){
    sessionSetup* s = sessionSetup::getSessionSetup();
    s->show();
}


void MainWindow::startCalibration(){
    CalibrationScreen* calibrationScreen = CalibrationScreen::getCalibrationScreen();
    Tracker* tracker;
    if(ui->trackerBox->currentIndex() == 0) tracker = (Tracker*)mouseTracker;
    else tracker = (Tracker*)TobiiEyeTracker::tobiiEyeTrackers[ui->trackerBox->currentIndex()-1];
    calibrationScreen->startCalibration(tracker);
}
