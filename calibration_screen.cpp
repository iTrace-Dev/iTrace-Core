#include "calibration_screen.hpp"

CalibrationScreen* CalibrationScreen::calibrationScreen = 0;

CalibrationScreen* CalibrationScreen::getCalibrationScreen(){
    if(!calibrationScreen){
        calibrationScreen = new CalibrationScreen();
    }
    return calibrationScreen;
}

CalibrationScreen::CalibrationScreen(QWidget *parent) : QWidget(parent){
    setWindowFlags(Qt::FramelessWindowHint|Qt::CustomizeWindowHint);
    resize(1000,1000);
    setWindowState(Qt::WindowFullScreen);
    points[0] = QPointF(0.1,0.1);
    points[1] = QPointF(0.5,0.1);
    points[2] = QPointF(0.9,0.1);
    points[3] = QPointF(0.1,0.5);
    points[4] = QPointF(0.5,0.5);
    points[5] = QPointF(0.9,0.5);
    points[6] = QPointF(0.1,0.9);
    points[7] = QPointF(0.5,0.9);
    points[8] = QPointF(0.9,0.9);

    timer = new QTimer(this);
    size = 50;
    connect(timer, SIGNAL(timeout()), this, SLOT(updatePosition()));
}

void CalibrationScreen::startCalibration(Tracker* selectedTracker){
    for(int i = 0; i < 9; i++)
    {
        QPointF tmp = points[i];
        int j = QRandomGenerator::system()->bounded(i, 9);
        points[i] = points[j];
        points[j] = tmp;
    }

    tracker = selectedTracker;
    tracker->enterCalibration();
    this->show();
    timer->start(16);
    t = 0;
    return;
}

void CalibrationScreen::stopCalibration(){
    tracker->leaveCalibration();
    timer->stop();
    this->hide();
}

void CalibrationScreen::paintEvent(QPaintEvent * /*event*/){
    if((t/100)%2 == 0){
        if((t%100) < 44){
            if((t%100) == 43) tracker->useCalibrationPoint(points[t/200].x(),points[t/200].y());
            size = size-1;
        }
        else if((t%100) >= 56) size = size + 1;

        x = (int)((points[t/200].x()*(float)width())-(size/2));
        y = (int)((points[t/200].y()*(float)height())-(size/2));
        dotx = (int)((points[t/200].x()*(float)width())-2);
        doty = (int)((points[t/200].y()*(float)height())-2);

    }else{
        QPointF direction(
                    ((points[((t/200)+1)%9].x()*width())-(points[t/200].x()*width()))*((float)(t%100)/100),
                    ((points[((t/200)+1)%9].y()*height())-(points[t/200].y()*height()))*((float)(t%100)/100)
                );
        x = (int)((points[t/200].x()*width())+direction.x())-(size/2);
        y = (int)((points[t/200].y()*height())+direction.y())-(size/2);
        dotx = (int)((points[t/200].x()*width())+direction.x())-2;
        doty = (int)((points[t/200].y()*height())+direction.y())-2;
    }
    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing, true);
    painter.setBrush(Qt::red);
    painter.setPen(Qt::red);
    painter.drawEllipse(x,y,size,size);
    painter.setBrush(Qt::black);
    painter.setPen(Qt::black);
    painter.drawEllipse(dotx,doty,4,4);
    if(t == 1699) stopCalibration();
}

void CalibrationScreen::updatePosition(){
    t++;
    t %= 1800;
    update();
}
