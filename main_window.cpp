#include "main_window.hpp"
#include "calibration_screen.hpp"
#include <QDebug>

MainWindow::MainWindow(QWidget *parent):
    QMainWindow(parent), ui(new Ui::MainWindow), trackerManager() {

    ui->setupUi(this);

    app_state = IDLE;

    connect(ui->startServerButton, SIGNAL(released()), this, SLOT(startTracker()));
    connect(ui->reticleBox, SIGNAL(stateChanged(int)), this, SLOT(toggleReticle()));
    connect(ui->calibrateButton, SIGNAL(released()), this, SLOT(startCalibration()));
    connect(ui->sessionButton, SIGNAL(released()), this, SLOT(showSessionSetup()));
    connect(ui->trackerBox, SIGNAL(currentTextChanged(QString)), this, SLOT(setActiveTracker()));

    this->setFixedSize(this->geometry().width(),this->geometry().height());

    std::vector<std::string> trackerNames = trackerManager.getTrackerNames();
    for (std::vector<std::string>::const_iterator it = trackerNames.begin(); it != trackerNames.end(); ++it) {
        ui->trackerBox->addItem(it->c_str());
    }
}

MainWindow::~MainWindow() {
    delete ui;
}

void MainWindow::setActiveTracker() {
    trackerManager.setActiveTracker(ui->trackerBox->currentText().toStdString());
}

void MainWindow::startTracker() {
    if (!(ui->xmlCheck->isChecked() && (!ui->jsonCheck->isChecked()))) {
        //Need to select and output format
        qDebug() << "Must select xml or json output";
        return;
    }

    if (app_state == IDLE) {
        ui->startServerButton->setText("Stop Tracker");
        trackerManager.startTracking();
        app_state = TRACKING;
    }
    else {
        ui->startServerButton->setText("Start Tracker");
        trackerManager.stopTracking();
        app_state = IDLE;
    }
}

void MainWindow::toggleReticle() {}
void MainWindow::displayData() {}
void MainWindow::showSessionSetup() {}

void MainWindow::startCalibration() {
    CalibrationScreen* calibrationScreen = CalibrationScreen::getCalibrationScreen();
    calibrationScreen->startCalibration(trackerManager.getActiveTracker());
}