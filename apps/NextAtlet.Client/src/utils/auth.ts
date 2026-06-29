import { cookies } from 'next/headers';
import { auth0 } from '@/lib/auth0'; // your auth0 client instance


export async function getServerCookies(): Promise<string> {
    const cookieStore = await cookies();
    return cookieStore
        .getAll()
        .map((c) => `${c.name}=${c.value}`)
        .join('; ');
}

export const checkLoggedIn = async () => {
    const session = auth0.getSession();
    return !!session
};