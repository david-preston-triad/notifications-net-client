﻿using Notify.Client;
using Notify.Models;
using Notify.Models.Responses;
using Notify.Exceptions;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NotifyIntegrationTests
{
    [TestClass]
    public class NotificationClientTests
    {
        private NotificationClient client;

        private String NOTIFY_API_URL = Environment.GetEnvironmentVariable("NOTIFY_API_URL");
        private String API_KEY = Environment.GetEnvironmentVariable("API_KEY");
        private String FUNCTIONAL_TEST_NUMBER = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_NUMBER");
        private String FUNCTIONAL_TEST_EMAIL = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_EMAIL");

        private String EMAIL_TEMPLATE_ID = Environment.GetEnvironmentVariable("EMAIL_TEMPLATE_ID");
        private String SMS_TEMPLATE_ID = Environment.GetEnvironmentVariable("SMS_TEMPLATE_ID");

        private String smsNotificationId;
        private String emailNotificationId;
        
        const String TEST_SUBJECT = "Functional Tests are good";
        const String TEST_EMAIL_BODY = "Hello someone\n\nFunctional test help make our world a better place";
        const String TEST_SMS_BODY = "Hello someone\n\nFunctional Tests make our world a better place";
        const String TEST_TEMPLATE_SMS_BODY = "Hello ((name))\n\nFunctional Tests make our world a better place";
        const String TEST_TEMPLATE_EMAIL_BODY = "Hello ((name))\n\nFunctional test help make our world a better place";

        [TestInitialize]
        [TestCategory("Integration")]
        public void TestInitialise()
        {
            this.client = new NotificationClient(NOTIFY_API_URL, API_KEY);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void SendSmsTestWithPersonalisation()
        {
            Dictionary<String, dynamic> personalisation = new Dictionary<String, dynamic>
            {
                { "name", "someone" }
            };

            SmsNotificationResponse response = 
                this.client.SendSms(FUNCTIONAL_TEST_NUMBER, SMS_TEMPLATE_ID, personalisation, "sample-test-ref");
            this.smsNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_SMS_BODY);

            Assert.IsNotNull(response.reference);
            Assert.AreEqual(response.reference, "sample-test-ref");
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetSMSNotificationWithIdReturnsNotification()
        {
            SendSmsTestWithPersonalisation();
            Notification notification = this.client.GetNotificationById(this.smsNotificationId);

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, this.smsNotificationId);

            Assert.IsNotNull(notification.body);
            Assert.AreEqual(notification.body, TEST_SMS_BODY);

            Assert.IsNotNull(notification.reference);
            Assert.AreEqual(notification.reference, "sample-test-ref");

            AssertNotification(notification);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void SendEmailTestWithPersonalisation()
        {
            Dictionary<String, dynamic> personalisation = new Dictionary<String, dynamic>
            {
                { "name", "someone" }
            };
			
            EmailNotificationResponse response = 
                this.client.SendEmail(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation);
            this.emailNotificationId = response.id;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.content.subject, TEST_SUBJECT);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetEmailNotificationWithIdReturnsNotification()
        {
            SendEmailTestWithPersonalisation();
            Notification notification = this.client.GetNotificationById(this.emailNotificationId);

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, this.emailNotificationId);

            Assert.IsNotNull(notification.body);
            Assert.AreEqual(notification.body, TEST_EMAIL_BODY);
            Assert.IsNotNull(notification.subject);
            Assert.AreEqual(notification.subject, "Functional Tests are good");

            AssertNotification(notification);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetAllNotifications()
        {
            NotificationList notificationsResponse = this.client.GetNotifications();
            Assert.IsNotNull(notificationsResponse);
            Assert.IsNotNull(notificationsResponse.notifications);

            List<Notification> notifications = notificationsResponse.notifications;

            foreach (Notification notification in notifications)
            {
                AssertNotification(notification);
            }

        }

        [TestMethod()]
        [TestCategory("Integration")]
        [ExpectedException(typeof(NotifyClientException), "A client was instantiated with an invalid key")]
        public void GetNotificationWithInvalidIdRaisesClientException()
        {
            try
            {
                this.client.GetNotificationById("fa5f0a6e-5293-49f1-b99f-3fade784382f");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "Status code 404. The following errors occured [\r\n  {\r\n    \"error\": \"NoResultFound\",\r\n    \"message\": \"No result found\"\r\n  }\r\n]");
                throw;
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        [ExpectedException(typeof(NotifyClientException), "A client was instantiated with an invalid key")]
        public void GetTemplateWithInvalidIdRaisesClientException()
        {
            try
            {
                this.client.GetTemplateById("fa5f0a6e-5293-49f1-b99f-3fade784382f");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "Status code 404. The following errors occured [\r\n  {\r\n    \"error\": \"NoResultFound\",\r\n    \"message\": \"No result found\"\r\n  }\r\n]");
                throw;
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetAllTemplates()
        {
            TemplateList templateList = this.client.GetTemplateList();
            Assert.IsNotNull(templateList);
            Assert.IsTrue(templateList.templates.Count > 0);

            foreach (TemplateResponse template in templateList.templates)
            {
                AssertTemplateResponse(template);
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetAllSMSTemplates()
        {
        	const String type = "sms";
            TemplateList templateList = this.client.GetTemplateList(type);
            Assert.IsNotNull(templateList);
            Assert.IsTrue(templateList.templates.Count > 0);

            foreach (TemplateResponse template in templateList.templates)
            {
                AssertTemplateResponse(template, type);
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetAllEmailTemplates()
        {
        	const String type = "email";
            TemplateList templateList = this.client.GetTemplateList(type);
            Assert.IsNotNull(templateList);
            Assert.IsTrue(templateList.templates.Count > 0);

            foreach (TemplateResponse template in templateList.templates)
            {
                AssertTemplateResponse(template, type);
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        [ExpectedException(typeof(NotifyClientException), "type invalid is not one of [sms, email, letter]")]
        public void GetAllInvalidTemplatesRaisesError()
        {
        	try {
	        	const String type = "invalid";
	            TemplateList templateList = this.client.GetTemplateList(type);
        	}
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "Status code 400. The following errors occured [\r\n  {\r\n    \"error\": \"ValidationError\",\r\n    \"message\": \"type invalid is not one of [sms, email, letter]\"\r\n  }\r\n]");
                throw;
            }
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetSMSTemplateWithId()
        {
            TemplateResponse template = this.client.GetTemplateById(SMS_TEMPLATE_ID);
            Assert.AreEqual(template.id, SMS_TEMPLATE_ID);
            Assert.AreEqual(template.body, TEST_TEMPLATE_SMS_BODY);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GetEmailTemplateWithId()
        {
            TemplateResponse template = this.client.GetTemplateById(EMAIL_TEMPLATE_ID);
            Assert.AreEqual(template.id, EMAIL_TEMPLATE_ID);
            Assert.AreEqual(template.body, TEST_TEMPLATE_EMAIL_BODY);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GenerateSMSPreviewWithPersonalisation()
        {
            Dictionary<String, dynamic> personalisation = new Dictionary<String, dynamic>
            {
                { "name", "someone" }
            };
			
            TemplatePreviewResponse response = 
                this.client.GenerateTemplatePreview(SMS_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.body, TEST_SMS_BODY);
            Assert.AreEqual(response.subject, null);
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void GenerateEmailPreviewWithPersonalisation()
        {
            Dictionary<String, dynamic> personalisation = new Dictionary<String, dynamic>
            {
                { "name", "someone" }
            };
			
            TemplatePreviewResponse response = 
                this.client.GenerateTemplatePreview(EMAIL_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.subject, TEST_SUBJECT);
        }

        public void AssertNotification(Notification notification)
        {
            Assert.IsNotNull(notification.type);
            String notificationType = notification.type;
            String[] allowedNotificationTypes = { "email", "sms" };
            CollectionAssert.Contains(allowedNotificationTypes, notificationType);
            if (notificationType.Equals("sms"))
            {
                Assert.IsNotNull(notification.phoneNumber);
            }
            else if (notificationType.Equals("email"))
            {
                Assert.IsNotNull(notification.emailAddress);
                Assert.IsNotNull(notification.subject);
            }

            Assert.IsNotNull(notification.body);
            Assert.IsNotNull(notification.createdAt);

            Assert.IsNotNull(notification.status);
            String notificationStatus = notification.status;
            String[] allowedStatusTypes = { "created", "sending", "delivered", "permanent-failure", "temporary-failure", "technical-failure" };
            CollectionAssert.Contains(allowedStatusTypes, notificationStatus);

            if (notificationStatus.Equals("delivered"))
            {
                Assert.IsNotNull(notification.completedAt);
            }

            AssertTemplate(notification.template);
        }

        public void AssertTemplate(Template template)
        {
            Assert.IsNotNull(template);
            Assert.IsNotNull(template.id);
            Assert.IsNotNull(template.uri);
            Assert.IsNotNull(template.version);
        }
        
        public void AssertTemplateResponse(TemplateResponse template, String type=null) 
        {
        	Assert.IsNotNull(template);
            Assert.IsNotNull(template.id);
            Assert.IsNotNull(template.version);
            Assert.IsNotNull(template.type);
            if (template.type.Equals("email") || (!string.IsNullOrEmpty(type) && type.Equals("email")))
            	Assert.IsNotNull(template.subject);
            Assert.IsNotNull(template.created_at);
            Assert.IsNotNull(template.created_by);
            Assert.IsNotNull(template.body);
        }

    }
}
