#include "status_window_display.hpp"
#include "session_manager.hpp"
#include <QDebug>

StatusWindowDisplay::StatusWindowDisplay(QWidget *parent) : QOpenGLWidget(parent) {
    timer = new QTimer(this);
    connect(timer, SIGNAL(timeout()), this, SLOT(update()));
    timer->start(50);
    xPos = 0; yPos = 0;
    gCol = 1; rCol = 1; bCol = 1;
}

StatusWindowDisplay::~StatusWindowDisplay() {

}

void StatusWindowDisplay::initializeGL() {
    glClearColor(0,0,0,1);
}

void StatusWindowDisplay::paintGL() {
    glClear(GL_COLOR_BUFFER_BIT);
    glColor3f(rCol, gCol, bCol);

    drawCircle(xPos + .08, -1*yPos, .05f, 12);
    drawCircle(xPos - .08, -1*yPos, .05f, 12);
    glFlush();

}

void StatusWindowDisplay::resizeGL(int w, int h) {
    glViewport(0,0,w,h);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
}

void StatusWindowDisplay::drawCircle(float cx, float cy, float r, int num_segments) {
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

    //set color
    if (gaze.rightValidity == 1) {
        if (gaze.user_pos_rightZ > 300 && gaze.user_pos_rightZ < 600) {
            gCol = 1;
            rCol = 0;
            bCol = 0;
        } else {
            gCol = 0;
            rCol = 1;
            bCol = 0;
        }
    }
}

void StatusWindowDisplay::update() {
    repaint();
}

