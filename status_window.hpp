#ifndef STATUS_WINDOW_HPP
#define STATUS_WINDOW_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include "ui_status_window.h"

namespace Ui {
 class StatusWindow;
}

class StatusWindow : public QDialog {
    Q_OBJECT

public:
    explicit StatusWindow(QWidget *parent = 0);
    ~StatusWindow();

private slots:
    void closeWindow();

private:
    Ui::StatusWindow *ui;

};

#endif // STATUS_WINDOW_HPP
