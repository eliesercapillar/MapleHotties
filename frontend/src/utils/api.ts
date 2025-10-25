export async function apiFetch(input: RequestInfo, init: RequestInit = {}) {
  const token = localStorage.getItem('token');
  const headers = {
    ...(init.headers ?? {}),
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  };

  const response = await fetch(input, { ...init, headers });
  if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
  return response;
}