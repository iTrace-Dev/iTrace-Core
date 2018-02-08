#ifndef SESSION_SETUP_HPP
#define SESSION_SETUP_HPP

#include <QObject>
#include <QWidget>
#include <QDialog>

namespace Ui {
 class SessionSetup;
}

class SessionSetup : public QDialog
{
    Q_OBJECT

public:
    explicit SessionSetup(QWidget *parent = 0);
    ~SessionSetup();

private:
    Ui::SessionSetup *ui;
};

#endif // SESSION_SETUP_HPP
