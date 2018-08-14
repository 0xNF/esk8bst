import * as Firebase from 'firebase';
import { firestore } from 'firebase';
import { FirebaseMatch } from 'src/models/dbTypes';

let db: firestore.Firestore;

function auth(){
  return Firebase.auth().signInAnonymously().catch(function(error) {
      //console.log(error.code);
      //console.log(error.message); 
  }).then((x: Firebase.auth.UserCredential) => {
    return x;
  });
}

let suid: string = "";
async function init(){
  // Initialize Firebase
  const config = {
    apiKey: "AIzaSyBhIoTIhLZQHpQbrjBJsqif36dYMgTYP68",
    authDomain: "esk8bst.firebaseapp.com",
    projectId: "esk8bst",
    storageBucket: "esk8bst.appspot.com",
    messagingSenderId: "135700105041"
  };
  Firebase.initializeApp(config);
  db = Firebase.firestore();
  db.settings({timestampsInSnapshots: true});
  const uid: Firebase.auth.UserCredential = await auth();
  console.log(uid.user!.uid);
  if(uid.user){
    suid = uid.user!.uid;
  }
  tadd(false, uid.user);
}

function tadd(test: boolean = false, ownerid: Firebase.User | null){
  if(test && ownerid && ownerid.uid){
    console.log("can add");
    const d2: FirebaseMatch  = {
      owner: "nf@gmail.com",
      matches: [
        {
          currency: "usd",
          price: 800,          
        }
      ]
    }
    db.collection("matches").add(
      d2
    ).catch((e) => {
      console.log("ayyyy");
      console.log(e);
    });
  } else {
    console.log("failed to add");
  }
}

function getAdd(){
  console.log("test");
  
  db.collection("matches").get().then((querySnapshot) => {
      querySnapshot.forEach((doc) => {
          const d: FirebaseMatch = doc.data() as FirebaseMatch;
          const i = d.owner;
          const ms = d.matches;
          console.log(`${doc.id}`);
          console.log(i);
          console.log(ms);
      });
  }).catch((e) => {
    // console.log("failed to retreive user matches");
    // console.log(e);
  })
}


async function doss(){
    await init();
    console.log(suid);
    getAdd();
  }

  
  export { doss as RegisterFirebase }