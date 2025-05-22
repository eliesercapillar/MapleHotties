<template>
  <!-- TODO: Colour scheme. -->
    <div class="bg-black-grey-radial min-h-screen flex flex-col">
        <main class="flex items-center justify-center flex-1">
          <Card class="w-[500px]">
            <CardHeader class="text-center">
              <CardTitle>Sign in to MapleTinder</CardTitle>
              <CardDescription>
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form class="pb-6">
                <div class="grid items-center w-full gap-4">
                  <div class="flex flex-col space-y-1.5">
                    <Label for="name">Email</Label>
                    <Input id="email" type="email" v-model="email"/>
                  </div>
                  <div class="flex flex-col space-y-1.5">
                    <Label for="framework">Password</Label>
                    <Input id="password" type="password" v-model="password"/>
                  </div>
                </div>
              </form>
              <Button @click="login" class="w-full mb-6">Sign in</Button>
              <div class="flex items-center mb-6">
                <div class="border-b-[1px] flex-1"></div>
                <span class="mx-2 flex-1 text-center">Or Sign in with</span>
                <div class="border-b-[1px] flex-1"></div>
              </div>
              <div class="flex items-center justify-center gap-2">
                <Button variant="outline" class="w-full mb-6">
                  <Icon icon="simple-icons:google"/>Google
                </Button>
                <Button @click="discordLogin"variant="outline" class="w-full mb-6">
                  <Icon icon="simple-icons:discord"/>Discord
                </Button>
              </div>
            </CardContent>
            <CardFooter class="flex justify-center px-6 pb-6">
              <span class="text-sm">
                Don't have an account? 
                <a href="/register" class="text-blue-400">Sign up!</a>
              </span>
            </CardFooter>
          </Card>
        </main>
    </div>
</template>

<script setup lang="ts">
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import Input  from '@/components/ui/input/Input.vue';
import Label  from '@/components/ui/label/Label.vue';
import Button from '@/components/ui/button/Button.vue';
import { Icon } from '@iconify/vue/dist/iconify.js';
import { ref } from "vue";
import router from "@/router"

const email = ref('');
const password = ref('');

async function login() {
  try {
    const url = `https://localhost:7235/auth/login`;
    //const url = `http://localhost:5051/auth/login`;
    const payload = {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: email.value,
        password: password.value
      })
    }

    const response = await fetch(url, payload);
    if (!response.ok) 
    {
      throw new Error(`${response.status}`);
    }
    
    const data = await response.json();
    localStorage.setItem('token', data.token);
    router.push('/main')
  }
  catch (err) {
    console.error("Failed to sign in:", err);
  }
}

function discordLogin() {
  //window.location.href = 'http://localhost:5051/auth/login/discord';
  window.location.href = 'https://localhost:7235/auth/login/discord';
}

</script>