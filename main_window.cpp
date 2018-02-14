#include "main_window.hpp"
#include "calibration_screen.hpp"
#include "gaze_data.hpp"
#include <QDebug>

MainWindow::MainWindow(QWidget *parent):
    QMainWindow(parent), ui(new Ui::MainWindow), trackerManager(), server(),
    reticle((QWidget*) this->parent()), sessionDialog((QWidget*) this->parent()) {

    qRegisterMetaType<std::string>();
    qRegisterMetaType<GazeData>();

    buffer = GazeBuffer::Instance();
    app_state = IDLE;

    ui->setupUi(this);
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
    buffer->Delete();

    if (ui)
        delete ui;
}

void MainWindow::setActiveTracker() {
    trackerManager.setActiveTracker(ui->trackerBox->currentText().toStdString());
}

void MainWindow::startTracker() {
    if (app_state == IDLE) {
        ui->startServerButton->setText("Stop Tracker");

        xml.setEnvironment(trackerManager.getActiveTracker()->trackerName());
        bufferHandler = new GazeHandler();
        QThreadPool::globalInstance()->start(bufferHandler);
        connect(bufferHandler, &GazeHandler::socketOut, &server, &Server::writeData);
        connect(bufferHandler, &GazeHandler::reticleOut, &reticle, &Reticle::moveReticle);
        connect(bufferHandler, &GazeHandler::xmlOut, &xml, &XMLWriter::writeResponse);

        trackerManager.startTracking();
        app_state = TRACKING;
    }
    else {
        ui->startServerButton->setText("Start Tracker");
        trackerManager.stopTracking();
        app_state = IDLE;
    }
}

void MainWindow::toggleReticle() {
    if (ui->reticleBox->isChecked()) {
        reticle.show();
    } else {
        reticle.hide();
    }
}

void MainWindow::displayData() {}

void MainWindow::showSessionSetup() {
    sessionDialog.show();
}

void MainWindow::startCalibration() {
    CalibrationScreen* calibrationScreen = CalibrationScreen::getCalibrationScreen();
    calibrationScreen->startCalibration(trackerManager.getActiveTracker());
}
