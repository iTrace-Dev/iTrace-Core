#include "websocket_server.hpp"
#include <QDebug>

WebsocketServer::WebsocketServer(QObject *parent): QObject(parent) {
    server = new QWebSocketServer("Browser Plugin Server", QWebSocketServer::NonSecureMode, this);
    connect(server, SIGNAL(newConnection()), this, SLOT(newConnection()));
    if (!server->listen(QHostAddress::Any, PORT)) {
        qDebug() << "Server could not start";
    }
}

void WebsocketServer::newConnection() {
    qDebug() << "WEBSOCKET CLIENT CONNECTED!";
    QWebSocket* client_conn = server->nextPendingConnection();
    clients.push_back(client_conn);
}

size_t WebsocketServer::clientCount() {
    return clients.size();
}

WebsocketServer::~WebsocketServer() {
    //Proper Socket Clean-up
    for (std::vector<QWebSocket*>::const_iterator it = clients.begin(); it != clients.end(); ++it) {
        (*it)->close();
        (*it)->deleteLater();
    }
    clients.clear();
    server->close();
    server->deleteLater();
}

void WebsocketServer::writeData(std::string value) {
    std::vector<QWebSocket*>::const_iterator it = clients.begin();
    while (it != clients.end()) {
        if ((*it)->state() == QAbstractSocket::UnconnectedState) {
            (*it)->close();
            (*it)->deleteLater();
            it = clients.erase(it);
        }
        else{
            (*it)->sendTextMessage(QString::fromStdString(value));
            ++it;
        }
    }
}
