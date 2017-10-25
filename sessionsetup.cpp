#include "sessionsetup.h"
#include "ui_sessionsetup.h"
using namespace std;

sessionSetup* sessionSetup::session = 0;
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
