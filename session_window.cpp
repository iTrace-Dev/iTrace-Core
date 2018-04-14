#include <QFileDialog>
#include <QString>
#include <QStandardPaths>
#include "session_window.hpp"
#include "session_manager.hpp"
#include <QDebug>

SessionWindow::SessionWindow(QWidget *parent) : QDialog(parent), ui(new Ui::SessionWindow) {
    ui->setupUi(this);
    SessionManager& session = SessionManager::Instance();

    connect(ui->buttonBox, SIGNAL(accepted()), this, SLOT(okClick()));
    connect(ui->pushButton, SIGNAL(released()), this, SLOT(directorySelect()));
}

void SessionWindow::okClick() {
    SessionManager& session = SessionManager::Instance();
    session.sessionSetup(ui->textEdit->toPlainText().toStdString(),
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

SessionWindow::~SessionWindow() {
    delete ui;
}
