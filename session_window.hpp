#ifndef SESSION_WINDOW_HPP
#define SESSION_WINDOW_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include "ui_session_window.h"

namespace Ui {
 class SessionWindow;
}

class SessionWindow : public QDialog
{
    Q_OBJECT

public:
    explicit SessionWindow(QWidget *parent = 0);
    ~SessionWindow();

private:
    Ui::SessionWindow *ui;
};

#endif // SESSION_WINDOW_HPP
