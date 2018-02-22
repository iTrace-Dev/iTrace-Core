#include "server.hpp"
#include "gaze_buffer.hpp"
#include <QDebug>

Server::Server(QObject *parent): QObject(parent) {
    server = new QTcpServer(this);
    connect(server, SIGNAL(newConnection()), this, SLOT(newConnection()));
    if (!server->listen(QHostAddress::Any, PORT)) {
        qDebug() << "Server could not start";
    }
}

Server::~Server() {
    //Proper Socket Clean-up
    while (!clients.empty()) {
        QTcpSocket* client = clients.back();
        client->close();
        client->deleteLater();
        clients.pop_back();
    }
    server->close();
    server->deleteLater();
}

void Server::newConnection() {
    qDebug() << "CLIENT CONNECTED!";
    QTcpSocket* client_conn = server->nextPendingConnection();
    connect(client_conn, SIGNAL(disconnected()), this, SLOT(clientDisconnect()));
    clients.push_back(client_conn);
}

void Server::clientDisconnect() {
    qDebug() << "CLIENT DISCONNECTED!";

    std::vector<QTcpSocket*>::iterator client = clients.begin();

    while (client != clients.end()) {
        if ((*client)->state() == QAbstractSocket::UnconnectedState) {
            QTcpSocket* lostClient = (*client);
            client = clients.erase(client);  //Remove the client from the clients we transmit to
            lostClient->deleteLater();       //Have Qt clean up the client
        }
    }
}

void Server::writeData(std::string value) {
    for (std::vector<QTcpSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->write(value.c_str());
        (*it)->flush();
        (*it)->waitForBytesWritten();
    }
}
