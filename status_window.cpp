#include "status_window.hpp"
#include "session_manager.hpp"

StatusWindow::StatusWindow(QWidget *parent) : QDialog(parent), ui(new Ui::StatusWindow) {
    ui->setupUi(this);
    //connect(ui->, SIGNAL(accepted()), this, SLOT(okClick()));
    //connect(ui->pushButton, SIGNAL(released()), this, SLOT(closeWindow()));
}

StatusWindow::~StatusWindow() {
    delete ui;
}

void StatusWindow::closeWindow() {
    this->close();
}

