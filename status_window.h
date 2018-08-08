#ifndef STATUS_WINDOW_H
#define STATUS_WINDOW_H

#include <QObject>
#include <QWidget>
#include <QDialog>
#include <QOpenGLWidget>
#include <gl/GLU.h>
#include <gl/GL.h>

namespace Ui {
 class StatusWindow;
}

class StatusWindow : public QOpenGLWidget {

public:
    StatusWindow(QWidget *parent = 0);
    ~StatusWindow();

protected:
    void initializeGL();
    void resizeGL(int w, int h);
    void paintGL();
};

#endif // STATUS_WINDOW_H
