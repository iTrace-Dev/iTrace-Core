#include "status_window.hpp"

StatusWindow::StatusWindow(QWidget *parent) : QDialog(parent), ui(new Ui::StatusWindow) {
    ui->setupUi(this);   
}

StatusWindow::~StatusWindow() {
    delete ui;
}

void StatusWindow::closeWindow() {
    this->close();
}



