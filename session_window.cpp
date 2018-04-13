#include <QFileDialog>
#include <QString>
#include <QStandardPaths>
#include "session_window.hpp"
#include <QDebug>

SessionWindow::SessionWindow(SessionManager *sessionManager, QWidget *parent) : QDialog(parent), ui(new Ui::SessionWindow)
{
    ui->setupUi(this);
    session = sessionManager;

    connect(ui->buttonBox, SIGNAL(accepted()), this, SLOT(okClick()));
    connect(ui->pushButton, SIGNAL(released()), this, SLOT(directorySelect()));
}

void SessionWindow::okClick() {
    session->sessionSetup(ui->textEdit->toPlainText().toStdString(),
                          ui->textEdit_2->toPlainText().toStdString(),
                          ui->textEdit_3->toPlainText().toStdString(),
                          ui->textEdit_4->toPlainText().toStdString());
}

void SessionWindow::directorySelect() {
    QString dir = QFileDialog::getExistingDirectory(this, tr("Open Directory"),
                                                QStandardPaths::standardLocations(QStandardPaths::HomeLocation).first(),
                                                QFileDialog::ShowDirsOnly
                                                | QFileDialog::DontResolveSymlinks);
    ui->textEdit_4->setText(dir);
}

SessionWindow::~SessionWindow()
{
    delete ui;
}
