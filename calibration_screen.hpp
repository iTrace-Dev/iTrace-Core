#ifndef CALIBRATION_SCREEN_HPP
#define CALIBRATION_SCREEN_HPP

#include <QWidget>
#include <QPainter>
#include <QTimer>
#include <QFile>
#include <QString>
#include <QXmlStreamReader>
#include <QMouseEvent>
#include <cstdlib>
#include <ctime>
#include "tracker.hpp"

class CalibrationScreen : public QWidget {
    Q_OBJECT

    public:
        static CalibrationScreen* getCalibrationScreen();
        void startCalibration(Tracker* tracker);
        void stopCalibration();

    public slots:
        void updatePosition();

    protected:
        void paintEvent(QPaintEvent* event) override;
        void mousePressEvent(QMouseEvent*  event) override;

    private:
        static CalibrationScreen* calibrationScreen;
        explicit CalibrationScreen(QWidget *parent = 0);
        QPointF points[9];
        QTimer * timer;
        Tracker* tracker;
        QXmlStreamReader reader;
        QList<QPointF> lPoints;
        QList<QPointF> rPoints;
        int t;
        int x;
        int y;
        int dotx;
        int doty;
        float size;
        bool complete;
};

#endif // CALIBRATION_SCREEN_HPP
