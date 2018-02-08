#ifndef RETICLE_HPP
#define RETICLE_HPP

#include <QWidget>
#include <QPainter>
#include <QPalette>

namespace Ui {
class Reticle;
}

class Reticle : public QWidget
{
    Q_OBJECT

public:
    explicit Reticle(QWidget *parent = 0);
    ~Reticle();

public slots:
    void moveReticle(double x, double y);

protected:
    void paintEvent(QPaintEvent *event) override;

private:
    Ui::Reticle *ui;
    int prevPoints[15][2];
    bool firstPoint = true;
    int curPoint = 0;
};

#endif // RETICLE_HPP
