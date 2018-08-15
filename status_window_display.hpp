#ifndef STATUS_WINDOW_DISPLAY_HPP
#define STATUS_WINDOW_DISPLAY_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include <QOpenGLWidget>
#include <gl/GLU.h>
#include <gl/GL.h>

namespace Ui {
 class StatusWindowDisplay;
}

class StatusWindowDisplay : public QOpenGLWidget {

public:
    StatusWindowDisplay(QWidget *parent = 0);
    ~StatusWindowDisplay();

protected:
    void initializeGL();
    void resizeGL(int w, int h);
    void paintGL();
};

#endif // STATUS_WINDOW_DISPLAY_HPP
