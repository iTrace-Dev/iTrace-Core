#include "reticle.hpp"
#include <QDebug>

Reticle::Reticle(QWidget *parent) : QWidget(parent), ui(new Ui::Reticle) {
    ui->setupUi(this);
    setAttribute(Qt::WA_TranslucentBackground);
    setAttribute(Qt::WA_QuitOnClose, false);
    this->setWindowFlags(Qt::FramelessWindowHint|Qt::WindowStaysOnTopHint);

    //Setup for efficent running average
    totalX = 0;
    totalY = 0;
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

    // No reason to do anything if it can't be seen...
    if (!(this->isVisible()))
        return;

    //Sum up all the x and y we have seen
    totalX += x;
    totalY += y;

    //Add them to the list of values we have seen
    xPoints.push_front(x);
    yPoints.push_front(y);

    /*
     * If we have enough points (MAX_NUM_POINTS) for desired smoothness
     * then we take an average of the points and move the reticle.
     *
     * To save re-totaling the points, we then just remove the oldest value from the
     * total and the lists (back) so the next time the function is called we only
     * evaluate the last MAX_NUM_POINTS.
     */
    if (xPoints.size() == MAX_NUM_POINTS) {
        double avgX = totalX/MAX_NUM_POINTS;
        double avgY = totalY/MAX_NUM_POINTS;
        this->move(avgX-(geometry().width()/2), avgY-(geometry().height()/2));

        //Remove oldest points from totals and lists
        totalX -= xPoints.back();
        totalY -= yPoints.back();
        xPoints.pop_back();
        yPoints.pop_back();
    }
}
