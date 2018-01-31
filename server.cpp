#include "server.hpp"
#include <QDebug>

Server::Server(QObject *parent): QObject(parent) {
    server = new QTcpServer(this);
    connect(server, SIGNAL(newConnection()), this, SLOT(newConnection()));
    if (!server->listen(QHostAddress::Any, PORT)) {
        qDebug() << "Server could not start";
    }
}

Server::~Server() {
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->close();
    }
    delete server;
}

void Server::newConnection() {
    clients.push_back(server->nextPendingConnection());
}

void Server::writeData(double value) {
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->write(value);
        (*it)->flush();
        (*it)->waitForBytesWritten();
    }
}
