#include "main_window.hpp"
#include "calibration_screen.hpp"
#include "gaze_data.hpp"
#include <QDesktopWidget>
#include <QDebug>


MainWindow::MainWindow(QWidget *parent):
    QMainWindow(parent), ui(new Ui::MainWindow), trackerManager(), socketServer(), websocketServer(), xml(nullptr),
    reticle((QWidget*) this->parent()), sessionDialog((QWidget*) this->parent()), statusWindow((QWidget*) this->parent()) {

    qRegisterMetaType<std::string>();
    qRegisterMetaType<GazeData>();

    app_state = IDLE;

    ui->setupUi(this);
    connect(ui->startServerButton, SIGNAL(released()), this, SLOT(startTracker()));
    connect(ui->reticleBox, SIGNAL(stateChanged(int)), this, SLOT(toggleReticle()));
    connect(ui->calibrateButton, SIGNAL(released()), this, SLOT(startCalibration()));
    connect(ui->sessionButton, SIGNAL(released()), this, SLOT(showSessionSetup()));
    connect(ui->trackerBox, SIGNAL(currentTextChanged(QString)), this, SLOT(setActiveTracker()));
    connect(ui->statusButton, SIGNAL(released()), this, SLOT(eyeStatus()));

    // Hide reticle checkbox.
    //  This might be useful to enable as a debugging feature.
    //  For now, just comment out to show.
    ui->reticleBox->hide();

    this->setFixedSize(this->geometry().width(),this->geometry().height());
    std::vector<std::string> trackerNames = trackerManager.getTrackerNames();
    for (std::vector<std::string>::const_iterator it = trackerNames.begin(); it != trackerNames.end(); ++it) {
        ui->trackerBox->addItem(it->c_str());
    }
}

// Called when user clicks the application close button in titlebar
void MainWindow::closeEvent(QCloseEvent *event) {

    // Need to clean-up any worker thread and resources before closing
    if (app_state == TRACKING)
        trackerManager.stopTracking();

    // Allow application to close
    event->accept();
}

MainWindow::~MainWindow() {
    if (ui == nullptr)
        delete ui;

    if (xml == nullptr)
        delete xml;
}

void MainWindow::setActiveTracker() {
    trackerManager.setActiveTracker(ui->trackerBox->currentText().toStdString());
}

void MainWindow::startTracker() {
    if (app_state == IDLE) {
        ui->startServerButton->setText("Stop Tracker");

        /* This should probably get refactored to where session manager deals with this logic
         * and remove most of this from mainwindow.
         */

        /* Start a session
            1) Give the session a unique ID
            2) Create a path based in the ID to store data
        */
        SessionManager& session = SessionManager::Instance();
        session.startSession();

        std::string sessionInfo = "session,";
        sessionInfo += session.getSessionPath() + '\n';

        //Notify web/socket clients of the session data location (this should be handled elsewhere)
        socketServer.writeData(sessionInfo);
        websocketServer.writeData(sessionInfo);

        // Determine screen dimensions before starting tracker (this causes issues when run from threads)
        //  iTrace always records from the primary desktop screen.
        //  Secondary screen should be used for study monitoring.
        QDesktopWidget* desktop = QApplication::desktop();
        QRect screen = desktop->screen(desktop->primaryScreen())->geometry();

        session.setScreenDimensions(screen.width(), screen.height());

        //Get an xmlwriter ready to write gaze and environment data from the core
        xml = new XMLWriter();
        xml->setEnvironment(trackerManager.getActiveTracker()->trackerName());

        bufferHandler = new GazeHandler();

        QThreadPool::globalInstance()->start(bufferHandler);
        connect(bufferHandler, &GazeHandler::socketOut, &socketServer, &SocketServer::writeData);
        connect(bufferHandler, &GazeHandler::websocketOut, &websocketServer, &WebsocketServer::writeData);
        connect(bufferHandler, &GazeHandler::reticleOut, &reticle, &Reticle::moveReticle);
        connect(bufferHandler, &GazeHandler::xmlOut, xml, &XMLWriter::writeResponse);
        connect(bufferHandler, &GazeHandler::eyeStatusOut, &statusWindow, &StatusWindow::getEyePos);

        trackerManager.startTracking();
        app_state = TRACKING;

        ui->reticleBox->setEnabled(false);
    }
    else {
        ui->startServerButton->setText("Start Tracker");
        trackerManager.stopTracking();
        app_state = IDLE;

        ui->reticleBox->setEnabled(true);
        delete xml;
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
    trackerManager.getActiveTracker();
    calibrationScreen->startCalibration(trackerManager.getActiveTracker());
}

void MainWindow::eyeStatus() {
    statusWindow.show();
}
