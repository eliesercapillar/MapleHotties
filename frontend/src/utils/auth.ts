// TODO: need to flush tokens when expired. also add token validation, not just checking if a token is present.
export function isLoggedIn() {
    return !!localStorage.getItem('token');
}