#include "status_window_display.hpp"
#include "session_manager.hpp"
#include <QTimer>
#include <QDebug>

StatusWindowDisplay::StatusWindowDisplay(QWidget *parent) : QOpenGLWidget(parent) {
    xPos = 0;
    yPos = 0;
}

StatusWindowDisplay::~StatusWindowDisplay() {

}

void StatusWindowDisplay::initializeGL() {
    glClearColor(0,0,0,1);
    glEnable(GL_DEPTH_TEST);
    glEnable(GL_LIGHT0);
    glEnable(GL_LIGHTING);
    glColorMaterial(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE);
    glEnable(GL_COLOR_MATERIAL);
}

void StatusWindowDisplay::paintGL() {
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    qDebug() << xPos << ", " << yPos;

    drawCircle(xPos + .08, yPos, .05f, 12);
    drawCircle(xPos - .08, yPos, .05f, 12);
    glFlush();
}

void StatusWindowDisplay::resizeGL(int w, int h) {
    glViewport(0,0,w,h);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
}

void StatusWindowDisplay::drawCircle(float cx, float cy, float r, int num_segments)
{
    glBegin(GL_TRIANGLE_FAN);
    for(int i = 0; i < num_segments; i++) {
        float theta = 2.0f * 3.1415926f * float(i) / float(num_segments);//get the current angle

        float x = r * cosf(theta);//calculate the x component
        float y = r * sinf(theta);//calculate the y component

        glVertex2f(x + cx, y + cy);//output vertex

    }
    glEnd();
}

void StatusWindowDisplay::setEyePos(GazeData gaze) {
    SessionManager& session = SessionManager::Instance();

    if (gaze.getCalculatedX() >= 0) {
        xPos = (gaze.getCalculatedX() / session.getScreenWidth()) * 2 - 1;
    }

    if (gaze.getCalculatedY() >= 0) {
        yPos = (gaze.getCalculatedY() / session.getScreenHeight()) * 2 - 1;
    }
}
