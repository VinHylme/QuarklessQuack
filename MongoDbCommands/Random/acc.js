db.getCollection("Account").deleteOne({
	"_id":ObjectId("5cc6cecf1c9d440000323f41")
})

db.getCollection("Account").find({})

db.getCollection("Account").insertOne({
	"Name": "admina",
	"Email": "yaaalaw@outlook.com",
    "Username": "sometest",
    "PremiumType" : "Ultra", 
    "LastLoggedIn":new Date(),
    "createdOn":new Date(),
    "TotalNumberOfInstagramAccounts": 2,
    "TotalNumberOfProfiles": 1
})