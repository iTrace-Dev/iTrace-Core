#include "reticle.hpp"
#include "ui_reticle.h"
#include <QDebug>

Reticle::Reticle(QWidget *parent) : QWidget(parent), ui(new Ui::Reticle) {
    ui->setupUi(this);
    setAttribute(Qt::WA_TranslucentBackground);
    setAttribute(Qt::WA_QuitOnClose, false);
    this->setWindowFlags(Qt::FramelessWindowHint|Qt::WindowStaysOnTopHint);
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

void Reticle::moveReticle(double x, double y){
    //qDebug(data);
    int newx = 0;
    int newy = 0;
    int avgx = 0;
    int avgy = 0;
    int size = sizeof(prevPoints) / sizeof(prevPoints[0]);

    newx = x;
    newy = y;

    if(newx > 0 && newy > 0){
        if(firstPoint){
            for(int j = 0; j < size; ++j){
                prevPoints[j][0] = newx;
                prevPoints[j][1] = newy;
            }
            firstPoint = false;
        }
        else{
            prevPoints[curPoint][0] = newx;
            prevPoints[curPoint][1] = newy;
            curPoint = (curPoint + 1) % size;
        }

        for(int j = 0; j < size; ++j){
            avgx += prevPoints[j][0];
            avgy += prevPoints[j][1];
        }
        avgx = avgx / size;
        avgy = avgy / size;
        this->move(avgx-(geometry().width()/2),avgy-(geometry().height()/2));
    }
}
