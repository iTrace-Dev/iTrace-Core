#ifndef STATUS_WINDOW_DISPLAY_HPP
#define STATUS_WINDOW_DISPLAY_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include <QOpenGLWidget>
#include <QOpenGLFunctions>
#include <gl/GLU.h>
#include <gl/GL.h>
#include <QTimer>
#include "gaze_data.hpp"

namespace Ui {
 class StatusWindowDisplay;
}

class StatusWindowDisplay : public QOpenGLWidget {
    Q_OBJECT

    public:
        StatusWindowDisplay(QWidget *parent = 0);
        ~StatusWindowDisplay();
        void setEyePos(GazeData gaze);
        double xPos;
        double yPos;

    public slots:
        void update();

    protected:
        void initializeGL() override;
        void resizeGL(int w, int h) override;
        void paintGL() override;
        void drawCircle(float cx, float cy, float r, int num_segments);

    private:
        float gCol;
        float rCol;
        float bCol;
        QTimer *timer;

};

#endif // STATUS_WINDOW_DISPLAY_HPP
