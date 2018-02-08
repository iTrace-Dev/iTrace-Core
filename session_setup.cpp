#include "session_setup.hpp"
#include "ui_sessionsetup.h"

SessionSetup::SessionSetup(QWidget *parent) : QDialog(parent), ui(new Ui::SessionSetup)
{
    ui->setupUi(this);
}

SessionSetup::~SessionSetup()
{
    delete ui;
}
