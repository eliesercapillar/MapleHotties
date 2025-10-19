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
                <Input type="email" v-model="email"/>
                <span class="text-sm" :class="isValidEmail ? 'text-green-500' : 'text-red-500'">Enter a valid email.</span>
              </div>
              <div class="flex flex-col space-y-1.5">
                <Label for="framework">Password</Label>
                <Input type="password" v-model="password"/>
              </div>
              <div class="flex flex-col space-y-1.5">
                <Label for="framework">Confim Password</Label>
                <Input type="password" v-model="confirmPassword"/>
              </div>
              <ul v-if="badPassword" class="text-sm mt-2 text-left space-y-1">
                <li :class="hasUppercase ? 'text-green-500' : 'text-red-500'">• At least one uppercase letter</li>
                <li :class="hasLowercase ? 'text-green-500' : 'text-red-500'">• At least one lowercase letter</li>
                <li :class="hasDigit ? 'text-green-500' : 'text-red-500'">• At least one number</li>
                <li :class="hasSpecialChar ? 'text-green-500' : 'text-red-500'">• At least one special character</li>
                <li :class="isLongEnough ? 'text-green-500' : 'text-red-500'">• Minimum 8 characters</li>
                <li :class="doesPassMatch ? 'text-green-500' : 'text-red-500'">• Passwords match</li>
              </ul>
            </div>
          </form>
          <Button @click="register" class="w-full mb-6" type="submit" :disabled=!isValidForm>Create Account</Button>
          <div class="flex items-center mb-6">
                <div class="border-b-[1px] flex-1"></div>
                <span class="mx-2 flex-1 text-center">Or Sign in with</span>
                <div class="border-b-[1px] flex-1"></div>
              </div>
              <div class="flex items-center justify-center gap-2">
                <Button variant="outline" class="w-full mb-6">
                  <Icon icon="simple-icons:google"/>Google
                </Button>
                <Button variant="outline" class="w-full mb-6">
                  <Icon icon="simple-icons:discord"/>Discord
                </Button>
              </div>
        </CardContent>
        <CardFooter class="flex flex-col justify-center px-6 pb-6">
          <span class="text-sm">
            Already have an account?
            <a href="/login" class="text-blue-400">Sign in!</a>
          </span>
          <span class="text-xs">
            By creating an account, you agree to our 
            <a href="/terms-" class="text-blue-400">Terms of Use</a>
            and
            <a href="/privacy-policy" class="text-blue-400">Privacy Policy</a>.
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
import { ref, computed } from "vue";
import router from "@/router"

const email = ref('');
const password = ref('');
const confirmPassword = ref('');

// Password validation checks
const hasUppercase = computed(() => /[A-Z]/.test(password.value))
const hasLowercase = computed(() => /[a-z]/.test(password.value))
const hasDigit = computed(() => /\d/.test(password.value))
const hasSpecialChar = computed(() => /[^a-zA-Z0-9]/.test(password.value))
const isLongEnough = computed(() => password.value.length >= 8)
const doesPassMatch = computed(() => password.value == confirmPassword.value)

const isValidEmail = computed(() => /\S+@\S+\.\S+/.test(email.value))

const isPasswordValid = computed(() =>
  hasUppercase.value &&
  hasLowercase.value &&
  hasDigit.value &&
  hasSpecialChar.value &&
  isLongEnough.value
)

const doPasswordsMatch = computed(() => password.value == confirmPassword.value)

const isValidForm = computed(() => isValidEmail && password.value.length > 0 && confirmPassword.value.length)

const badPassword = ref(false);

async function register() {
  try {
    if (!isPasswordValid.value || !doPasswordsMatch.value) {
        badPassword.value = true;
        // error.value = 'Password does not meet the required criteria.'
        return
    }


    const url = `https://localhost:7235/auth/register`;
    // const url = `http://localhost:5051/api/Auth/register`;
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
