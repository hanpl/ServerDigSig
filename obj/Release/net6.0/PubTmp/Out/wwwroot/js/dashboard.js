"use strict"
var connection = new signalR.HubConnectionBuilder().withUrl("/dashboardHub").build();


$(function () {
    connection.start().then(function () {
        /*    document.getElementById("sendButton").disabled = false;*/
        console.log("Connected signalr !");
        //loadOrder();
        //InvokeWorkOrder();
    }).catch(function (err) {
        return console.error(err.toString());
    });
})






