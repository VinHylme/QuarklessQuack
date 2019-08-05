  import {AES} from 'crypto-js';
  /*
  Store the calculated ciphertext and IV here, so we can decrypt the message later.
  */
  let ciphertext;
  let iv;

  /*
  Get the encoded message, encrypt it and display a representation
  of the ciphertext in the "Ciphertext" element.
  */
let keyp = "LO=12391U24192CMADA.12381ADAH%$QLQM12£!11K95DASKNFCKABSDFG"
export function encrypt(message){
  return AES.encrypt(message,keyp);
}
export function EncryptPredefined(message)
{
  var key = "D1C6ED3C6049221408A4EB634F20E393270ABC31145C20A6"; //replace with your key
  var iv = "A80198517884F820FE558857688728CE"; //replace with your IV

  var cipher = crypto.createCipheriv('aes256', key, iv)
  var crypted = cipher.update(message, 'utf8', 'base64')
  crypted += cipher.final('base64');
  return crypted;
}
export async function encryptMessage(key, message) {
    let enc = new TextEncoder();
    let encoded = enc.encode(message);
    // The iv must never be reused with a given key.
    iv = window.crypto.getRandomValues(new Uint8Array(16));
    ciphertext = await window.crypto.subtle.encrypt(
      {
        name: "AES-CBC",
        iv
      },
      key,
      encoded
    );
    let str = new TextDecoder().decode(ciphertext); 
    let str2 = new TextDecoder().decode(iv);

    return {
      encrypted: str2,
      cipher: str
    }
    // let buffer = new Uint8Array(ciphertext, 0, 5);
    // return  `${buffer}...[${ciphertext.byteLength} bytes total]`;
  }

  /*
  Fetch the ciphertext and decrypt it.
  Write the decrypted message into the "Decrypted" box.
  */
export async function decryptMessage(key) {
    let decrypted = await window.crypto.subtle.decrypt(
      {
        name: "AES-CBC",
        iv
      },
      key,
      ciphertext
    );

    let dec = new TextDecoder();
    return dec.decode(decrypted);
  }

  /*
  Generate an encryption key, then set up event listeners
  on the "Encrypt" and "Decrypt" buttons.
  */

export async function generateKey(){
  var key_ = {}
    await window.crypto.subtle.generateKey({
      name: "AES-CBC",
      length: 256
    },
    true,
    ["encrypt", "decrypt"]
    ).then((key)=>{
      key_ = key; 
    })
    return key_;
}
