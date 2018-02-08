#include "session_setup.hpp"
#include "ui_sessionsetup.h"

SessionSetup* session = 0;

void SessionSetup::createSessionSetup(QWidget *parent){
    if(!session){
        session = new SessionSetup(parent);
    }
}

SessionSetup::SessionSetup(QWidget *parent) : QDialog(parent), ui(new Ui::SessionSetup)
{
    ui->setupUi(this);
}

SessionSetup::~SessionSetup()
{
    delete ui;
}

SessionSetup* SessionSetup::getSessionSetup(){
    return session;
}
