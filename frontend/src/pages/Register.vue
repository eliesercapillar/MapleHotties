<template>
  <!-- TODO: Colour scheme. -->
  <div class="bg-black-grey-radial min-h-screen flex flex-col">
    <main class="flex items-center justify-center flex-1">
      <Card class="w-[500px]">
        <CardHeader class="text-center">
          <CardTitle>Create an Account</CardTitle>
          <CardDescription> </CardDescription>
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
              <div class="flex flex-col space-y-1.5">
                <Label for="framework">Confim Password</Label>
                <Input id="password" type="password" v-model="confirmPassword"/>
              </div>
            </div>
          </form>
          <Button @click="register" class="w-full mb-6">Create Account</Button>
        </CardContent>
        <CardFooter class="flex justify-center px-6 pb-6">
          <span class="text-sm">
            Already have an account?
            <a href="/login" class="text-blue-400">Sign in!</a>
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
} from "@/components/ui/card";
import Input from "@/components/ui/input/Input.vue";
import Label from "@/components/ui/label/Label.vue";
import Button from "@/components/ui/button/Button.vue";
import { ref } from "vue";
import router from "@/router"

const email = ref('');
const password = ref('');
const confirmPassword = ref('');

async function register() {
  try {
    const url = `http://localhost:5051/api/Auth/register`;
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

      router.push('/login')
    }
    catch (err) {
      console.error("Failed to create new account:", err);
    }
}
</script>
