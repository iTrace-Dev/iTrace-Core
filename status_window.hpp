#ifndef STATUS_WINDOW_HPP
#define STATUS_WINDOW_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include "ui_status_window.h"
#include "status_window_display.hpp"
#include "gaze_data.hpp"

namespace Ui {
 class StatusWindow;
}

class StatusWindow : public QDialog {
    Q_OBJECT

    public:
        explicit StatusWindow(QWidget *parent = 0);
        ~StatusWindow();

    public slots:
        void getEyePos(GazeData gaze);

    private slots:
        void closeWindow();

    private:
        Ui::StatusWindow *ui;
        int pos;
};

#endif // STATUS_WINDOW_HPP
