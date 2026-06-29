'use client';

import { useSearchParams } from 'next/navigation';
import { paths } from '@/config/paths';


const RegisterPage = () => {
  const searchParams = useSearchParams();
  const redirectTo = searchParams?.get('redirectTo');

  const returnTo = redirectTo
    ? decodeURIComponent(redirectTo)
    : paths.home.getHref();

  return (
    <a
      href={`/auth/login?screen_hint=signup&returnTo=${encodeURIComponent(returnTo)}`}
      className="w-full text-center inline-block px-6 py-3 bg-gradient-to-b from-[#2d2d42] to-[#161620] hover:opacity-90 text-white font-medium rounded-full text-[14px] transition-opacity"
    >
      Register
    </a>
  );
};

export default RegisterPage;
