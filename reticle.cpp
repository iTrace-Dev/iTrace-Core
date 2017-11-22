#include "reticle.h"
#include "ui_reticle.h"
#include <sstream>
#include <string>

using namespace std;

Reticle* Reticle::reticle = 0;

void Reticle::createReticle(QWidget *parent){
    if(!reticle){
        reticle = new Reticle(parent);
    }
}

Reticle* Reticle::getReticle(){
    return reticle;
}

Reticle::Reticle(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::Reticle)
{
    ui->setupUi(this);
    setAttribute(Qt::WA_TranslucentBackground);
    setAttribute(Qt::WA_QuitOnClose, false);
    this->setWindowFlags(Qt::FramelessWindowHint|Qt::WindowStaysOnTopHint);
    socket = new QTcpSocket(this);
    connect(socket, SIGNAL(readyRead()), this, SLOT(moveReticle()));
    socket->connectToHost("localhost",8080,QIODevice::ReadOnly);
}

Reticle::~Reticle()
{
    delete ui;
}

void Reticle::paintEvent(QPaintEvent* /*event*/){
    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing, true);
    QColor red(255,0,0);
    QPen pen(red);
    pen.setWidth(5);
    painter.setBrush(Qt::NoBrush);
    painter.setPen(pen);
    painter.drawEllipse(5,5,geometry().width()-10,geometry().height()-10);
}

void Reticle::moveReticle(){
    QDataStream in(socket);
    in.setVersion(QDataStream::Qt_5_8);
    char* data = new char[100];
    in.readRawData(data,100);
    std::stringstream ss;
    ss << data;
    int newx;
    int newy;
    int i=0;
    /*
    while(data[i] != ',') i++;
    string xstring = ss.str().substr(0,i);
    newx = stoi(xstring);
    string ystring = ss.str().substr(i+1);
    newy = stoi(ystring);
    if(newx > 0 && newy > 0) this->move(newx-(geometry().width()/2),newy-(geometry().height()/2));
    */
}
