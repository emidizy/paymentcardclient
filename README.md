# paymentcardexplorer
Client for consuming messages published by PaymentCardExplorer service.

Requirements: 
• Visual studio 2017 or later 
• .Net core 2.2 or later 
• RabbitMQ 3.7+ (local or cloud hosted)

Description: This project is made up of two applications; A payment card inquiry application (.Net Core Web Service) and a receiver application (Console application).

• The payment card inquiry application (card inquiry microservice) looks up credit and debit card meta data from a third party API (https://binlist.net/) using a payment card’s Issuer Identification Number and publishes specific details of the returned card details to a RabbitMQ queue for the receiver application to consume.

• This service listens for events (messages) published to the queue, verifies the message using a tag and displays the message on the console.



Usage Guide 

Follow the steps below before you run this application; 

• Open the project’s appsettings file & ensure the RabbitMQ broker configurations are same as on your environment. 

Once done with the above steps, run this application (advised to keep the receiver application running at same time with the payment card inquiry application so that you can get real time Publish - Ack events).

Assumptions:

All dependencies (runtime, RabbitMQ) are available and properly configured on your environment and the third party card inquiry API (https://binlist.net) is always available and returns card details for a given valid card IIN (Issuer Identification Number).

Contact: ediala94@gmail.com

