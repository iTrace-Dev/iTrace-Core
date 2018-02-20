#ifndef RETICLE_HPP
#define RETICLE_HPP

#include <QWidget>
#include <QPainter>
#include <QPalette>
#include <list>
#include "ui_reticle.h" //Auto-generated

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
    void moveReticle(int x, int y);

protected:
    void paintEvent(QPaintEvent *event) override;

private:
    const size_t MAX_NUM_POINTS = 15;
    Ui::Reticle *ui;
    std::list<double> xPoints;
    std::list<double> yPoints;
    double totalX;
    double totalY;
};

#endif // RETICLE_HPP
