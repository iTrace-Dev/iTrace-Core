#ifndef STATUS_WINDOW_DISPLAY_HPP
#define STATUS_WINDOW_DISPLAY_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include <QOpenGLWidget>
#include <gl/GLU.h>
#include <gl/GL.h>
#include "gaze_data.hpp"

namespace Ui {
 class StatusWindowDisplay;
}

class StatusWindowDisplay : public QOpenGLWidget {
    Q_OBJECT

    public:
        StatusWindowDisplay(QWidget *parent = 0);
        ~StatusWindowDisplay();
        double xPos;
        double yPos;

    public slots:
        void setEyePos(GazeData gaze);

    protected:
        void initializeGL();
        void resizeGL(int w, int h);
        void paintGL();
        void drawCircle(float cx, float cy, float r, int num_segments);

};

#endif // STATUS_WINDOW_DISPLAY_HPP
