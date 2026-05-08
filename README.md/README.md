DocuSign HR Onboarding Integration Service 

Project Overview 

This project implements a backend service that automates the employee onboarding workflow using DocuSign. It accepts employee data, applies routing rules, initiates a DocuSign envelope, and exposes APIs to track the document status                                                                                                    

                         ┌──────────────────────────┐ 
                         │        HR System         │ 
                         └─────────────┬────────────┘ 
                                       │ 
                                       ▼(POST onboarding) 
                         ┌──────────────────────────┐ 
                         │           API            │ 
                         └─────────────┬────────────┘ 
                                       │ 
                                       ▼(apply business rules) 
                         ┌──────────────────────────┐ 
                         │      Business Logic      │ 
                         └─────────────┬────────────┘ 
                                       │ 
                                       ▼(create envelope request) 
                         ┌──────────────────────────┐ 
                         │       DocuSign API       │ 
                         └─────────────┬────────────┘ 
                                       │ 
                                       ▼(send envelope) 
                         ┌──────────────────────────┐ 
                         │     Recipient Emails     │ 
                         └─────────────┬────────────┘ 
                                       │ 
                                       ▼(signing happens) 
                         ┌──────────────────────────┐ 
                         │     Envelope Status      │ 
                         └─────────────┬────────────┘ 
                                                            

Solution Summary 

The service acts as a middleware between an HR onboarding system and DocuSign. It determines signing order based on employee role and manages the full envelope lifecycle. 

 

                           ┌───────────────┐ 
                           │     Start     │ 
                           └───────┬───────┘ 
                                   │ 
                                   ▼ 
                                   
                           ┌───────────────┐ 
                           │ Employee Data │ 
                           └───────┬───────┘ 
                                   │ 
                                   ▼ 
                                   
                           ┌───────────────┐ 
                           │ Check Position│ 
                           └───────┬───────┘ 
                                   │ 
                                   ▼ 
                                   
                     ┌───────────────────────────┐ 
                     │     Manager or Above?     │ 
                     └───────────┬───────────────┘ 
                                 │ 
                                 
                    ┌────────────┴────────────┐ 
                    │                         │ 
                   Yes                       No 
                    │                         │ 
                    ▼                         ▼ 
                    
        ┌───────────────────────┐   ┌───────────────────────┐ 
        │ Add Department Head   │   │ Skip Department Head │ 
        └─────────────┬─────────┘   └─────────────┬─────────┘ 
                      │                           │ 
                      └──────────────┬────────────┘ 
                                     ▼ 
                                     
                           ┌───────────────┐ 
                           │    HR Added   │ 
                           └───────┬───────┘ 
                                   │ 
                                   ▼ 
                                   
                           ┌───────────────┐ 
                           │ Employee Added│ 
                           └───────┬───────┘ 
                                   │ 
                                   ▼ 
                                   
                     ┌───────────────────────────┐ 
                     │   Create & Send Envelope  │ 
                     └───────────┬───────────────┘ 
                                 │ 
                                 ▼ 
                                 
                           ┌───────────────┐ 
                           │  Track Status │ 
                           └───────────────┘ 

Setup Instructions 

1.Prerequisites 

    .NET 8 SDK installed 
    
    DocuSign Developer (Demo) Account 
    
    Postman installed 
    
    A DocuSign Template with roles: 
    
      -HR Manager 
      
      -Employee 
      
      -Dept Head 

 

2.Clone the Repository 

    -git clone <repo-url> 
    
    -cd docusign-onboarding-backend 


3.Configure DocuSign Credentials 

    -Create a file : appsettings.Development.json 

    -Add: { 
    
      "DocuSign": { 

    "IntegrationKey": "<your-integration-key>", 

    "UserId": "<your-api-username-guid>", 

    "AuthServer": "account-d.docusign.com", 

    "PrivateKeyPath": "private.key", 

    "AccountId": "<your-account-id>"   

    } 

    } 


What each value means: 

    -IntegrationKey → From DocuSign → Settings → Apps & Keys 
    
    -UserId → API User (GUID) 
    
    -AuthServer → Always account-d.docusign.com for demo 
    
    -PrivateKeyPath → Path to RSA private key 
    
    -AccountId → DocuSign account ID (numeric) 


4.Add Your Private Key 

    -Place RSA private key file in the project root: /private.key 


5. Configure Your DocuSign Template 

        -Copy the Template ID 
        
        -Paste it into Program.cs ( string templateId = "template-id"); 


6. Run the Application 

       -dotnet run 


7. Test Using Postman 

Import the file : Employee Onboarding.postman_collection 

This collection includes: 

      - POST /start-onboarding 

       -GET /envelope-status/{envelopeId} 

        Example : { 

    "employeeName": "Test", 
  
    "employeeEmail": “test@example.com", 
  
    "jobLevel": "Manager", 
  
    "deptHeadName": "Test2", 
  
    "deptHeadEmail": "test2@exampe.com" 
  
    } 


7. Copy the returned envelopeId and call: 

          -GET /envelope-status/{envelopeId} 


DocuSign Developer Account Setup 

Follow these steps to configure DocuSign: 


01.Create a Developer Account 

    -Sign up at developers.docusign.com for a free sandbox account. 

 

02.Create an Integration Key  

    -Navigate to Settings → Apps & Keys → Add App to generate your Integration Key. 

 

03.Enable JWT Authentication 

      -Generate RSA Private and public key and enable 'Account Impersonation' under Authentication settings.  

 

04. Get Your API Username 

        -Copy the API Username (GUID) from user profile; this is required for JWT impersonation. 

 

05. Request application consent 

        -Before making any API calls using JWT Grant, we must get user’s consent for app to impersonate them. 
        -To get this consent, open the below Sample URI in embedded browser, replacing the value of client_id with integration key, scope and redirect URI 

        https://account-d.docusign.com/oauth/auth? 
    
        response_type=code 
    
        &scope= <scope> 
    
        &client_id= <integration key> 
    
        &redirect_uri= <redirect URI> 

        Now user will be prompted to sign in to their account and give consent for app to impersonate them. After user grants permission, app will be able to use the OAuth JWT Grant flow to impersonate them and make API calls. 

 

06.Create a Template 

    -Upload onboarding PDF and add template roles: HR Manager, Employee, Dept Head. 

    -And add Mail body ,subject line and text fields and signatures according to roles. 

    -Provide template access to the impersonated user. 

 

07.Test Using Demo Environment 

    -Use [https://demo.docusign.net] for all API calls during development 

API Endpoints 

    POST /start-onboarding --> Creates a DocuSign envelope using a template and dynamic routing rules. 
    
    GET /envelope-status/{envelopeId}---> Returns real time envelope metadata from DocuSign. 


Business Rules 

    Standard Employee: HR → Employee 
     
    Manager Level and Above: HR → Employee → Department Head 


Assumptions 

- HR signer email is fixed 
- Single onboarding template is used 
- Department head email is provided in request 
- Demo environment is used 


Known Limitations 

- No UI included 
- Simplified role based logic 
- No retry mechanism for failed API calls 
 
Tech Stack 

    C# 
    
    Postman 
    
    JWT oAuth 2.0 
    
    DocuSign eSignature  Rest API 

 
