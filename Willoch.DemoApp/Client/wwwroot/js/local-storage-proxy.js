export function Retrieve(key) {
    return localStorage.getItem(key);
}
export function Store(key, value) {    
    localStorage.setItem(key, value);
}
export function Remove(key) {
    localStorage.removeItem(key);
}
export function RemoveAll() {
    localStorage.clear();
}