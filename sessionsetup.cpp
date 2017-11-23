#include "sessionsetup.h"
#include "ui_sessionsetup.h"
using namespace std;

sessionSetup* session = 0;

void sessionSetup::createSessionSetup(QWidget *parent){
    if(!session){
        session = new sessionSetup(parent);
    }

}

sessionSetup::sessionSetup(QWidget *parent) :
    QDialog(parent),
    ui(new Ui::sessionSetup)
{
    ui->setupUi(this);
}

sessionSetup::~sessionSetup()
{
    delete ui;
}

sessionSetup* sessionSetup::getSessionSetup(){
    return session;
}
