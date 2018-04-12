#ifndef SESSION_WINDOW_HPP
#define SESSION_WINDOW_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>
#include "session_manager.hpp"
#include "ui_session_window.h"

namespace Ui {
 class SessionWindow;
}

class SessionWindow : public QDialog
{
    Q_OBJECT

public:
    explicit SessionWindow(SessionManager *sessionManager, QWidget *parent = 0);
    ~SessionWindow();

private slots:
    void okClick();
    void directorySelect();

private:
    Ui::SessionWindow *ui;
    SessionManager *session;
};

#endif // SESSION_WINDOW_HPP
