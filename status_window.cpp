#include "status_window.hpp"

StatusWindow::StatusWindow(QWidget *parent) : QDialog(parent), ui(new Ui::StatusWindow) {
    ui->setupUi(this);
    pos = 45;
}

StatusWindow::~StatusWindow() {
    delete ui;
}

void StatusWindow::closeWindow() {
    this->close();
}

void StatusWindow::getEyePos(GazeData gaze){
    ui->openGLWidget->setEyePos(gaze);

    if (gaze.rightValidity == 1) {
        pos = gaze.user_pos_rightZ / 100;
        ui->verticalSlider->setValue(pos);
    }
}

