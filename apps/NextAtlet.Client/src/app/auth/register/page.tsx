'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import { useState } from 'react';

import { paths } from '@/config/paths';
import { RegisterForm } from '@/features/auth/components/register-form';
import { useTeams } from '@/features/teams/api/get-teams';

const RegisterPage = () => {
  const router = useRouter();

  const searchParams = useSearchParams();
  const redirectTo = searchParams?.get('redirectTo');

  const [chooseTeam, setChooseTeam] = useState(false);

  const teamsQuery = useTeams({
    queryConfig: {
      enabled: chooseTeam,
    },
  });

  return (
    <a
      href="/auth/logout"
      className="w-full text-center inline-block px-6 py-3 bg-linear-to-b from-[#2d2d42] to-[#161620] hover:opacity-90 text-white font-medium rounded-full text-[14px] transition-opacity"
    >
      Logout
    </a>
  );
};

export default RegisterPage;
