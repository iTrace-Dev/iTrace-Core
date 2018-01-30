#ifndef RETICLE_H
#define RETICLE_H

#include <QWidget>
#include <QPainter>
#include <QPalette>
#include <QLocalServer>
#include <QLocalSocket>
#include <QTcpServer>
#include <QTcpSocket>

namespace Ui {
class Reticle;
}

class Reticle : public QWidget
{
    Q_OBJECT

public:
    explicit Reticle(QWidget *parent = 0);
    ~Reticle();
    static Reticle* getReticle();
    static void createReticle(QWidget *parent = 0);
public slots:
    void moveReticle();
protected:
    void paintEvent(QPaintEvent *event) override;

private:
    Ui::Reticle *ui;
    QTcpSocket* socket;
    static Reticle* reticle;
    int prevPoints[15][2];
    bool firstPoint = true;
    int curPoint = 0;
};

#endif // RETICLE_H
