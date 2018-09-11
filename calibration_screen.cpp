#include "calibration_screen.hpp"
#include "session_manager.hpp"
#include <QDesktopWidget>
#include <QApplication>

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
    points[0] = QPointF(0.1,0.1);
    points[1] = QPointF(0.5,0.1);
    points[2] = QPointF(0.9,0.1);
    points[3] = QPointF(0.1,0.5);
    points[4] = QPointF(0.5,0.5);
    points[5] = QPointF(0.9,0.5);
    points[6] = QPointF(0.1,0.9);
    points[7] = QPointF(0.5,0.9);
    points[8] = QPointF(0.9,0.9);

    srand(time(NULL));
    timer = new QTimer(this);
    size = 50;
    complete = false;
    connect(timer, SIGNAL(timeout()), this, SLOT(updatePosition()));
}

void CalibrationScreen::startCalibration(Tracker* selectedTracker){
    SessionManager::Instance().generateCalibrationID();
    setWindowState(Qt::WindowFullScreen);
    for(int i = 0; i < 9; i++)
    {
        QPointF tmp = points[i];
        int j = rand() % (9 - i) + i;
        points[i] = points[j];
        points[j] = tmp;
    }

    complete = false;
    tracker = selectedTracker;
    tracker->enterCalibration();
    this->show();
    // Move the calibration screen so it always appears on the primary display (where tracking will take place)
    QDesktopWidget* desktop = QApplication::desktop();
    QRect screen = desktop->screen(desktop->primaryScreen())->geometry();
    this->move(screen.x(), screen.y());
    timer->start(16);
    t = 0;
}

void CalibrationScreen::stopCalibration(){
    tracker->leaveCalibration();
    complete = true;

    SessionManager& session = SessionManager::Instance();
    QString calFile = QString::fromStdString(session.getStudyPath() + QDir::separator().toLatin1() + "calibration" +
                          QDir::separator().toLatin1() + SessionManager::Instance().getCalibrationID() + ".xml");

    //read in calibration values and store the points
    QFile dataFile(calFile);
    if(dataFile.exists()) {
        dataFile.open(QIODevice::ReadWrite);
        reader.setDevice(&dataFile);
        QXmlStreamAttributes attributes;

        while (!reader.atEnd()) {
            reader.readNext();
            if(reader.name().toString() == "sample") {
                attributes = reader.attributes();
                if(attributes.value("left_validity") == "1") {
                    int ptX = (int)((attributes.value("left_x").toFloat()*(float)width())-2);
                    int ptY = (int)((attributes.value("left_y").toFloat()*(float)height())-2);
                    lPoints.append(QPointF(ptX, ptY));
                }
                if(attributes.value("right_validity") == "1") {
                    int ptX = (int)((attributes.value("right_x").toFloat()*(float)width())-2);
                    int ptY = (int)((attributes.value("right_y").toFloat()*(float)height())-2);
                    rPoints.append(QPointF(ptX, ptY));
                }
            }
        }
    }
    dataFile.close();
}

void CalibrationScreen::paintEvent(QPaintEvent * /*event*/){
    if(!complete) {
        //Paint the calibration process
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
        //Draw each calibration point
        painter.setBrush(Qt::black);
        painter.setPen(Qt::black);
        for(int i = 0; i < 9; ++i) {
            int ptX = (int)((points[i].x()*(float)width())-2);
            int ptY = (int)((points[i].y()*(float)height())-2);
            painter.drawEllipse(ptX,ptY,4,4);
        }
        //Draw the reticle
        painter.setBrush(Qt::red);
        painter.setPen(Qt::red);
        painter.drawEllipse(x,y,size,size);
        painter.setBrush(Qt::black);
        painter.setPen(Qt::black);
        painter.drawEllipse(dotx,doty,4,4);
    } else {
        //Paint the calibration results
        QPainter painter(this);
        painter.setRenderHint(QPainter::Antialiasing, true);
        //Draw a radius around each point
        painter.setPen(Qt::black); //black outline
        for(int i = 0; i < 9; ++i) {
            int ptX = (int)((points[i].x()*(float)width())-40);
            int ptY = (int)((points[i].y()*(float)height())-40);
            painter.drawEllipse(ptX,ptY,80,80);
        }
        //Draw each calibration point
        painter.setBrush(Qt::black); //black fill
        for(int i = 0; i < 9; ++i) {
            int ptX = (int)((points[i].x()*(float)width())-2);
            int ptY = (int)((points[i].y()*(float)height())-2);
            painter.drawEllipse(ptX,ptY,4,4);
        }
        //Draw left and right eye calibration points
        painter.setBrush(Qt::blue);
        painter.setPen(Qt::blue);
        for(int i = 0; i < lPoints.size(); ++i) {
            int ptX = (int)(lPoints[i].x());
            int ptY = (int)(lPoints[i].y());
            painter.drawEllipse(ptX,ptY,4,4);
        }
        painter.setBrush(Qt::red);
        painter.setPen(Qt::red);
        for(int i = 0; i < rPoints.size(); ++i) {
            int ptX = (int)(rPoints[i].x());
            int ptY = (int)(rPoints[i].y());
            painter.drawEllipse(ptX,ptY,4,4);
        }
    }
}

void CalibrationScreen::mousePressEvent(QMouseEvent *event) {
    if(complete && event->button() == Qt::LeftButton) {
        timer->stop();
        this->showNormal();
        this->close();
    }
}

void CalibrationScreen::updatePosition(){
    t++;
    t %= 1800;
    if (t >= 1699 && !complete) {
        stopCalibration();
    }
    else {
        update();
    }
}
