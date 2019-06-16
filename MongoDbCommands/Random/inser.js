db.getCollection("Theme").insertOne({
	"Account_Id":ObjectId("51e0373c6f35b1826f47e9a0"),
	"Colors": [{
            "Red": 155,
            "Blue": 143,
            "Green": 55,
            "Alpha": 255,
        }],
        "Perctange": 90
})
db.getCollection("Theme").find({})

db.getCollection("Theme").updateOne(
{"_id":ObjectId("5cc6f709792437e09148ca10")},
{"$set":{"Account_Id":ObjectId("5cc6f85b3a8df535bd31b885")}})