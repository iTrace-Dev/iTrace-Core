#ifndef SESSIONSETUP_H
#define SESSIONSETUP_H

#include <QDialog>

namespace Ui {
 class sessionSetup;
}

class sessionSetup : public QDialog
{
    Q_OBJECT

public:
    explicit sessionSetup(QWidget *parent = 0);
    ~sessionSetup();
    static void createSessionSetup(QWidget *parent = 0);
    static sessionSetup* getSessionSetup();

private:
    Ui::sessionSetup *ui;
};

#endif // SESSIONSETUP_H
