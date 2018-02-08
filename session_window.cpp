#include "session_window.hpp"

SessionWindow::SessionWindow(QWidget *parent) : QDialog(parent), ui(new Ui::SessionWindow)
{
    ui->setupUi(this);
}

SessionWindow::~SessionWindow()
{
    delete ui;
}
