db.getCollection("Proxies").insertOne(
	{
	    "Account_Id":ObjectId("51e0373c6f35b1826f47e9a0"),
	    "Address": "123.123.12.1",
        "Port": 1313,
        "NeedServerAuth": true,
        "Username": "testtest",
        "Password": "pass",
        "UseProxyHttp": true,
    }
)
